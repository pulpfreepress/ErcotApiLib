# ErcotApiLib
The beginnings of client API library that connects to ERCOT SOAP Services

## What You Need To Connect To An ERCOT SOAP Service

- To successfully connect to an ERCOT service you need a **valid client SSL certificate issued by ERCOT** and **supporting SSL Certs** located here:
[Digital Certificate Security Information](http://www.ercot.com/services/mdt/webservices)

- To use these certs with this library, place them in a folder named Certificate (folder name can be changed via application settings). Also place the folder with the certs in the ErcotUnitTests/bin/debug and /release folders to successfully run tests.


