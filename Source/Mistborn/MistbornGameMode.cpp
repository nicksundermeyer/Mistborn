// Copyright 1998-2019 Epic Games, Inc. All Rights Reserved.

#include "MistbornGameMode.h"
#include "MistbornHUD.h"
#include "MistbornCharacter.h"
#include "UObject/ConstructorHelpers.h"

AMistbornGameMode::AMistbornGameMode()
	: Super()
{
	// set default pawn class to our Blueprinted character
	static ConstructorHelpers::FClassFinder<APawn> PlayerPawnClassFinder(TEXT("/Game/FirstPersonCPP/Blueprints/FirstPersonCharacter"));
	DefaultPawnClass = PlayerPawnClassFinder.Class;

	// use our custom HUD class
	HUDClass = AMistbornHUD::StaticClass();
}
