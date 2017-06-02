# This is the DotNet Core documentation

1. Configuration File.
2. Mandatory

## Configuration File.
For configure your application you need to configure the appsettings.json file : 

``` json
{   
  "logPath":"the file path for your log",
  "pluginPath":"the file path for your plugin",
  "ErrorCode": {
      "FR":{
        "1":"Le token de l'utilisateur existe deja",
        "2":"L'utilisateur existe deja"
      },
      "EN":{
        "1":"The token already exist",
        "2":"User already exist"
      }
  },
  "email":"email where will be send the notification",
  "SQL":{
    "connection":"server=localhost;user id=database_user;password=pwd_database;persistsecurityinfo=True;port=3306;database=database_name;SslMode=None"
  }
}
```

## Mandatory
You need to add the ***file path for the logPath***, if the file is not created he will be created.  

