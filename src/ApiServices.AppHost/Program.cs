var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.skTestApis>("sktestpApis");

var apiServicePets = builder.AddProject<Projects.PetStoreAPI>("apiservicepets")
    .WithHttpEndpoint(port: 8100) ;
var apiServiceStore = builder.AddProject<Projects.SuperHeroesAPI>("apiservicesuperheroes")
    .WithHttpEndpoint(port: 8188) ;

builder.Build().Run();

