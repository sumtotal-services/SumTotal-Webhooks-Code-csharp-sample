<p align="center" width="100%">
    <img src="https://marketplace.sumtotalsystems.com/content/images/vendor/SumTotal_logo.png"> 
</p>

# SumTotal Sample Code to Demonstrate Webhook/Event Usage

## Pre-requisites:
1. Microsoft Visual Studio 2019
2. IIS (Internet Information Services) is Enabled

## Setup Guide:
1. Download the Source Code from GitHub Location
2. Open the Solution in VS 2019 in administration Mode
3. Select the Solution to run in IIS Mode 
  - in VS 2019, you will have the option to select the project to run in different methods like IIS, IISExpress, etc - select IIS in dropdown, it will be present below Tools option
4. Run the Solution in IIS 
  - it will host the webhooklisterner in IIS and opens default browser with url : http://localhost/webhooklistener/ - you can change the url in launchsettings.json
5. Now the solution is ready to receive the api calls
  - you can hit the solution using the url in launchsettings.json or you can give your machinename in place of localhost in url

## Steps:
1. provide the webhooklisterner url  in webhooks configuration page
  - example: http://yourmachinename/webhooklistener/api/listener/listenevent or http://localhost/webhooklistener/api/listener/listenevent
  - if you are using http, make sure webhooks processor host and this sample solution hosted in same domain/network - otherwise url wont hit the project when any event triggers
2. trigger any event
3. it will call the post method "ListenEvent" in listenerController.cs
4. sends the response back to webhooks solution by capturing request details - shows the payload details and request.headers in log file
  - log file will be generated at C:/Data/Temp/webhooklistener-{Date}.json
5. if you want to validate the payload signature value in request.header , copy the secretKey from webhookEndpoint in UI and update the secretKey in appsettings.json
  - by default, secretKey value is empty in appsettings.json
  - trigger any event and it will show the signature is matched or not in response
  - if secretKey in appsettings.json is empty : response body in UI will be "Success and not validated the secret key as appsettings_secretkey is empty"
  - if secretKey in appsettings is updated correctly and if generated signature matched with request header signature, response body will be "Success and validated the appsettings_secretkey with the payload signature and result is matched and appsettings_secretkey is : xxxxx"
  - if secretKey in appsettings is not updated correctly or if generated signature not matched with request header signature, response body will be "Success and validated the appsettings_secretkey with the payload signature and result is NOT matched and appsettings_secretkey is : xxxxx"

