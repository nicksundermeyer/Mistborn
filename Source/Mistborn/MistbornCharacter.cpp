// Copyright 1998-2019 Epic Games, Inc. All Rights Reserved.

#include "MistbornCharacter.h"
#include "MistbornProjectile.h"
#include "Animation/AnimInstance.h"
#include "Camera/CameraComponent.h"
#include "Components/CapsuleComponent.h"
#include "Components/InputComponent.h"
#include "GameFramework/InputSettings.h"
#include "Misc/App.h"
#include "Engine.h"

#define print(text) if (GEngine) GEngine->AddOnScreenDebugMessage(-1, 1.5, FColor::White,text)

//////////////////////////////////////////////////////////////////////////
// AMistbornCharacter

AMistbornCharacter::AMistbornCharacter()
{
	// Create a CameraComponent	
	FirstPersonCameraComponent = CreateDefaultSubobject<UCameraComponent>(TEXT("FirstPersonCamera"));
	FirstPersonCameraComponent->SetupAttachment(GetCapsuleComponent());
	FirstPersonCameraComponent->AddRelativeLocation(FVector(0.0f, 0.0f, 100.0f));
	FirstPersonCameraComponent->bUsePawnControlRotation = true;
}

void AMistbornCharacter::BeginPlay()
{
	// Call the base class  
	Super::BeginPlay();
	isGrounded = true;
}

//////////////////////////////////////////////////////////////////////////
// Input

void AMistbornCharacter::SetupPlayerInputComponent(class UInputComponent* PlayerInputComponent)
{
	// set up gameplay key bindings
	check(PlayerInputComponent);

	// Bind jump events
	PlayerInputComponent->BindAction("Jump", IE_Pressed, this, &AMistbornCharacter::Jump);
	PlayerInputComponent->BindAction("Jump", IE_Released, this, &AMistbornCharacter::StopJumping);

	// Bind push and pull
	PlayerInputComponent->BindAction("Push", IE_Pressed, this, &AMistbornCharacter::OnPush);
	PlayerInputComponent->BindAction("Pull", IE_Pressed, this, &AMistbornCharacter::OnPullStart);
	PlayerInputComponent->BindAction("Pull", IE_Released, this, &AMistbornCharacter::OnPullEnd);

	// Bind movement events
	PlayerInputComponent->BindAxis("MoveForward", this, &AMistbornCharacter::MoveForward);
	PlayerInputComponent->BindAxis("MoveRight", this, &AMistbornCharacter::MoveRight);

	// We have 2 versions of the rotation bindings to handle different kinds of devices differently
	// "turn" handles devices that provide an absolute delta, such as a mouse.
	// "turnrate" is for devices that we choose to treat as a rate of change, such as an analog joystick
	PlayerInputComponent->BindAxis("Turn", this, &APawn::AddControllerYawInput);
	PlayerInputComponent->BindAxis("TurnRate", this, &AMistbornCharacter::TurnAtRate);
	PlayerInputComponent->BindAxis("LookUp", this, &APawn::AddControllerPitchInput);
	PlayerInputComponent->BindAxis("LookUpRate", this, &AMistbornCharacter::LookUpAtRate);
}

void AMistbornCharacter::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);

	// pull object towards player
	if(PulledObject != nullptr)
	{
		UStaticMeshComponent* MeshRoot = Cast<UStaticMeshComponent>(PulledObject->GetRootComponent());

		FVector Start = FirstPersonCameraComponent->GetComponentLocation();
		FVector ForwardVector = FirstPersonCameraComponent->GetForwardVector();
		FVector End = Start + (ForwardVector * 300.0f);

		FHitResult OutSweepHitResult;
		FVector Interpolation = FMath::VInterpConstantTo(PulledObject->GetActorLocation(), End, FApp::GetDeltaTime(), PullStrength/(PulledObject->GetActorLocation()-End).Size() + (1/Cast<UStaticMeshComponent>(PulledObject->GetRootComponent())->GetMass()));

		PulledObject->SetActorLocation(Interpolation, false, &OutSweepHitResult, ETeleportType::None);
		if((PulledObject->GetActorLocation()-End).Size() < 100.0f)
		{
			Cast<UStaticMeshComponent>(PulledObject->GetRootComponent())->SetEnableGravity(false);
		}
	}

	// launching
	if(launching)
	{
		FHitResult hit;

		if(Raycast(&hit, LaunchRadius, LaunchDistance, FVector(0, 0, -1)))
		{
			if(hit.GetActor()->ActorHasTag("Metal"))
			{
				ACharacter::LaunchCharacter(FVector(0, 0, LaunchStrength), true, true);
			}
		}
	}
}

void AMistbornCharacter::OnPush()
{
	DropObject();

	FHitResult Hit;
	
	if(Raycast(&Hit, PowerRadius, PowerDistance, FirstPersonCameraComponent->GetForwardVector()))
	{
		if(Hit.GetActor()->IsRootComponentMovable() && Hit.GetActor()->ActorHasTag("Metal")) 
		{
			UStaticMeshComponent* MeshRoot = Cast<UStaticMeshComponent>(Hit.GetActor()->GetRootComponent());
			
			float Distance = (Hit.GetActor()->GetActorLocation() - FirstPersonCameraComponent->GetComponentLocation()).Size();
			FVector Force = (FirstPersonCameraComponent->GetForwardVector()*100000000.0f*PushStrength)/FMath::Sqrt(MeshRoot->GetMass() + FMath::Pow(Distance, 2.0f));
			MeshRoot->AddForce(Force);
		}
	}
}

void AMistbornCharacter::OnPullStart()
{
	FHitResult Hit;

	if(Raycast(&Hit, PowerRadius, PowerDistance, FirstPersonCameraComponent->GetForwardVector()))
	{
		if(Hit.GetActor()->IsRootComponentMovable() && Hit.GetActor()->ActorHasTag("Metal")) 
		{
			// disable actor physics and set reference
			PulledObject = Hit.GetActor();
			UStaticMeshComponent* MeshRoot = Cast<UStaticMeshComponent>(PulledObject->GetRootComponent());
			MeshRoot->SetPhysicsAngularVelocity(FVector::ZeroVector);
		}
	}
}

void AMistbornCharacter::OnPullEnd()
{
	DropObject();
}

void AMistbornCharacter::DropObject()
{
	if(PulledObject != nullptr)
	{
		UStaticMeshComponent* MeshRoot = Cast<UStaticMeshComponent>(PulledObject->GetRootComponent());
		MeshRoot->SetPhysicsLinearVelocity(FVector::ZeroVector);
		MeshRoot->SetEnableGravity(true);
		PulledObject = nullptr;
	}
}

bool AMistbornCharacter::Raycast(FHitResult *Hit, float radius, float distance, FVector direction)
{
	FVector Start = FirstPersonCameraComponent->GetComponentLocation();
	FVector End = Start + (direction * distance);
	FQuat Rot = FQuat::MakeFromEuler(FVector(0, 0, 0));

	// DrawDebugLine(GetWorld(), Start, End, FColor::Green, false, 2.0f);

	FCollisionShape sphere = FCollisionShape::MakeSphere(radius);
	FCollisionQueryParams queryParams;
	queryParams.AddIgnoredActor(GetUniqueID()); // ignore self
	FCollisionResponseParams responseParams;
	FHitResult hit;

	FCollisionQueryParams CollisionParams;
	if(GetWorld()->SweepSingleByChannel(*Hit, Start, End, Rot, ECC_WorldDynamic, sphere, queryParams, responseParams))
	{
		return true;
	}
	else {
		return false;
	}
}

void AMistbornCharacter::MoveForward(float Value)
{
	if (Value != 0.0f)
	{
		// add movement in that direction
		AddMovementInput(GetActorForwardVector(), Value);
	}
}

void AMistbornCharacter::MoveRight(float Value)
{
	if (Value != 0.0f)
	{
		// add movement in that direction
		AddMovementInput(GetActorRightVector(), Value);
	}
}

void AMistbornCharacter::TurnAtRate(float Rate)
{
	// calculate delta for this frame from the rate information
	AddControllerYawInput(Rate * BaseTurnRate * GetWorld()->GetDeltaSeconds());
}

void AMistbornCharacter::LookUpAtRate(float Rate)
{
	// calculate delta for this frame from the rate information
	AddControllerPitchInput(Rate * BaseLookUpRate * GetWorld()->GetDeltaSeconds());
}

void AMistbornCharacter::Jump()
{
	if(isGrounded)
	{
		ACharacter::Jump();
		isGrounded = false;
	}
	else
	{
		launching = true;
	}
}

void AMistbornCharacter::StopJumping()
{
	ACharacter::StopJumping();
	launching = false;
}

void AMistbornCharacter::Landed(const FHitResult& hit)
{
	ACharacter::Landed(hit);
	isGrounded = true;
}