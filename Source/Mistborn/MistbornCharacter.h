// Copyright 1998-2019 Epic Games, Inc. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Character.h"
#include "MistbornCharacter.generated.h"

class UInputComponent;

UCLASS(config=Game)
class AMistbornCharacter : public ACharacter
{
	GENERATED_BODY()

public:
	AMistbornCharacter();

protected:
	virtual void BeginPlay();

	AActor * PulledObject;

public:
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category=Powers)
	float PushStrength;

	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category=Powers)
	float PullStrength;

	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category=Powers)
	float PowerDistance;

	/** Base turn rate, in deg/sec. Other scaling may affect final turn rate. */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category=Camera)
	float BaseTurnRate;

	/** Base look up/down rate, in deg/sec. Other scaling may affect final rate. */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category=Camera)
	float BaseLookUpRate;

	/** First person camera */
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = Camera, meta = (AllowPrivateAccess = "true"))
	class UCameraComponent* FirstPersonCameraComponent;

protected:

	// Called every frame
	virtual void Tick( float DeltaTime ) override;
	
	// Pushing objects
	void OnPush();

	// Pulling objects
	void OnPullStart();
	void OnPullEnd();
	
	// Helper to drop objects
	void DropObject();

	// Helper functiont to raycast
	bool RaycastForward(FHitResult *Hit);

	/** Handles moving forward/backward */
	void MoveForward(float Val);

	/** Handles stafing movement, left and right */
	void MoveRight(float Val);

	/**
	 * Called via input to turn at a given rate.
	 * @param Rate	This is a normalized rate, i.e. 1.0 means 100% of desired turn rate
	 */
	void TurnAtRate(float Rate);

	/**
	 * Called via input to turn look up/down at a given rate.
	 * @param Rate	This is a normalized rate, i.e. 1.0 means 100% of desired turn rate
	 */
	void LookUpAtRate(float Rate);

	virtual void Jump() override;

	virtual void StopJumping() override;

	
protected:
	// APawn interface
	virtual void SetupPlayerInputComponent(UInputComponent* InputComponent) override;
	// End of APawn interface

public:
	/** Returns FirstPersonCameraComponent subobject **/
	FORCEINLINE class UCameraComponent* GetFirstPersonCameraComponent() const { return FirstPersonCameraComponent; }

};

