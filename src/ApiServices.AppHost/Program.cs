var builder = DistributedApplication.CreateBuilder(args);

// var apiServicePets = builder.AddProject<Projects.PetStoreAPI>("apiservicepets")
//     .WithHttpEndpoint(port: 8100) ;
// var apiServiceStore = builder.AddProject<Projects.SuperHeroesAPI>("apiservicesuperheroes")
//     .WithHttpEndpoint(port: 8188) ;

builder.AddProject<Projects.skTestApis>("sktestpApis");

builder.Build().Run();

