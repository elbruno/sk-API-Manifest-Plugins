using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

var apiServicePets = builder.AddProject<PetStoreAPI>("apiservicepets")
    .WithHttpEndpoint(port: 8100) ;
var apiServiceStore = builder.AddProject<SuperHeroesAPI>("apiservicesuperheroes")
    .WithHttpEndpoint(port: 8188) ;

// builder.AddProject<Projects.AspireSample_Web>("webfrontend")
//     .WithExternalHttpEndpoints()
//     .WithReference(cache)
//     .WithReference(apiService);


builder.Build().Run();

