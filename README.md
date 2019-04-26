# Overview

This repository is a companion to the presentation and slides here https://www.slideshare.net/MarkArteaga/serverless-computing-with-azure-functions-and-xamarin

All commits start from building the application but if you run the latest commit you can run end to end demo.

## Get It Working

You require the following

1. Make sure you have the lastet Visual Studio 2017 with 
   1. Xamarin libs installed
   2. Azure Functions SDK
2. Azure Account
   1. Need an active Azure account, if don't have one create a free one https://azure.microsoft.com/en-ca/free/
   2. [Create a Azure Storage resource](https://docs.microsoft.com/en-us/azure/storage/common/storage-create-storage-account)
3. [Ngrok.io](Ngrok.io) - this is required to point your local function to be accessible from your mobile phone

To get the code running

1. Under src open up RedBit.XamServerless.sln
2. Replace keys for table storage
   1. In BlogManager.cs line 23 the `_connectionString` variable
   2. In TableManager.cs Line 20 the `_connectionString` variable
3. Change the ngrok url for uploading and status endpoints
4. Line 115 of MainPageViewModel.cs change the `BASE_URL`



## Disclaimer

This is released as is as sample code that I use for a presentation. If there is an issue please ping me on twitter or create an issue here but there is no guarantee for a fix and it works on my machine :)

If you would like me to debug your code, we can get one of our team members at [RedBit](http://www.redbitdev.com) to look at your issues.