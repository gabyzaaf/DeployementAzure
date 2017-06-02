# This documentation explain the Authentification webservices:

1. Webservice Type : 

The webservice is a ***POST***

2. The url :
 ***http://localhost:5000/api/user/admin/auth/***

3. The parameters are : 

|  email | password   |
|---|---| 
|  string |  string |

4. Sample Result : 

``` json
{   
  "sessionId": "27",
  "name": null, 
  "email": "juliec17@gmail.com",
  "password": null
}
```

5. If the error is about credentials : 

``` json
{
  "code":1,
  "content":"email ou password incorrect",
  "success":false	
}
