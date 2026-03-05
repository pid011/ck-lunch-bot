var builder = DistributedApplication.CreateBuilder(args);

var consumerApiKey = builder.AddParameter("consumerApiKey", secret: true);
var consumerSecretKey = builder.AddParameter("consumerSecretKey", secret: true);
var accessToken = builder.AddParameter("accessToken", secret: true);
var accessTokenSecret = builder.AddParameter("accessTokenSecret", secret: true);

builder.AddAzureFunctionsProject<Projects.CKLunchBot_Functions>("cklunchbot-functions")
    .WithEnvironment("Credentials__ConsumerApiKey", consumerApiKey)
    .WithEnvironment("Credentials__ConsumerSecretKey", consumerSecretKey)
    .WithEnvironment("Credentials__AccessToken", accessToken)
    .WithEnvironment("Credentials__AccessTokenSecret", accessTokenSecret);

builder.Build().Run();
