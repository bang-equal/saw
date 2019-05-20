# <span style="color:red;">S</span>erverless <span style="color:red;">A</span>SP.NET <span style="color:red;">W</span>ebAPI 

|   |   |
| ------------- | ------------- |
| Framework   | __ASP.NET Core 2.1.x__ |
| | |
| Security  | __ASP.NET Core Identity 2.1.x__  |
| | __JWT Bearer Authentication__  |
| | __Claims Based Authorization__  |
| | |
| Data Access ORM  | __ASP.NET Entity Framework Core 2.1.x__  |
| | |
| Database  | __Postgres 10.6__  |
| |  |
| Cloud Services  | __AWS Lambda__  |
|   | __AWS API Gateway__  |
|   | __AWS RDS__  |

### WHAT ###

A lightweight, cross-platform, and robust JSON RESTful web service. Start CRUDing to your PostgreSQL database right away thanks to data migration script that seeds initial data!

### WHY ###

You need an open source backend solution for your web application and building it on ASP.NET Core makes a lot of sense. For starters it is written in C# and thanks to Microsoft's commitment to open source and cross platform, it is lightweight and cost effective on AWS Lambda serverless platform. If you are skeptical about adopting the framework, consider the many years .NET developers have spent building RESTful HTTP APIs on the Microsoft stack. This track record has allowed for many improvements and enhancements over the years. Now that .NET is open source, embrace the advantages of building your JSON Web API on ASP.NET Core. For example:

* __Model Binding:__ map request data (form values, route data, query string parameters, HTTP headers) into objects defined in the action method's parameter.

* __Attribute Routing:__ decorate action methods with attributes that define your API routes.

* __Dependency Injection:__ give action methods the ability to request needed services through their constructor.

* __Security Features:__ many current tools and libraries to choose for managing security, including features to prevent security breaches.

* __Membership System:__ built in and robust login functionality. User names, passwords, and profile data are securely stored on your database.

### HOW ###

This project contains a full working example of a severless ASP.NET Web API. Use these instructions to run the example in your own AWS environment. 

#### PREREQUISITES ####
* [.NET Core 2.1](https://dotnet.microsoft.com/download/dotnet-core/2.1)
* [Postgresql 10](https://www.postgresql.org/about/news/1786/) database instance, you can use a cloud database service like [AWS RDS](https://docs.aws.amazon.com/AmazonRDS/latest/UserGuide/USER_CreatePostgreSQLInstance.html) or run your own server on localhost. You will need to build a Connection String and note value __${CONNECTIONSTRING}__ for later use.
```
User ID=${USERID};Password=${PASSWORD};Host=${SERVER};Port=5432;Database=Blog;Pooling=true;
```

* [Amazon Web Services (AWS)](https://aws.amazon.com/) account. Be sure to have generated [access keys](https://docs.aws.amazon.com/general/latest/gr/managing-aws-access-keys.html) for your aws account user. Not recommended to use your root user though, instead create an [IAM](https://docs.aws.amazon.com/IAM/latest/UserGuide/best-practices.html#create-iam-users) user.
* [AWS Command Line Interface ](https://aws.amazon.com/cli/)

#### PROCEDURE ####

1. Verify installation of dotnet-core.
```
dotnet --version
```

2. Configure aws-cli by providing AWS Access Key ID, AWS Secret Access Key, Default region name __${REGION__} and Default output format.
```
aws configure
```

3. Clone project
```
git clone https://github.com/bang-equal/saw
```

2. Navigate directory
```
cd saw/src/saw
```

3. Restore dependencies
```
dotnet restore
```

4. Install Amazon.Lambda.Tools Global Tools
```
dotnet tool install -g Amazon.Lambda.Tools
```

5. Create S3 bucket where application is uploaded, make sure bucket name is unique.
````
aws s3api create-bucket --bucket saw-11052019
````

6. Before running command below and deploying application to AWS Lambda, verify [aws-lambda-tools-defaults.json](src/saw/aws-lambda-tools-defaults.json) has correct value for region. You will be prompted to enter a CloudFormation Stack Name (enter any name) and S3 Bucket (enter bucket name created previously). Be patient while the application builds and uploads.
```
dotnet lambda deploy-serverless
```

7. Now that the lambda function has been created, display a list of all functions and find the newly created function.
```
aws lambda list-functions
```

Note FunctionArn value __${FUNCTIONARN}__ and FunctionName __${FUNCTIONNAME}__ values because they will be needed later.
```
{
    "Functions": [
        {
            "FunctionName": "saw-14112019-AspNetCoreFunction-1768RDJQEX0NX",
            "FunctionArn": "arn:aws:lambda:us-east-1:448834931284:function:saw-14112019-AspNetCoreFunction-1768RDJQEX0NX",
            "Runtime": "dotnetcore2.1",
            "Role": "arn:aws:iam::448834931284:role/saw-14112019-AspNetCoreFunctionRole-1EXV2TEVCX5S0",
            "Handler": "saw::saw.LambdaEntryPoint::FunctionHandlerAsync",
            "CodeSize": 1345066,
            "Description": "",
            "Timeout": 30,
            "MemorySize": 256,
            "LastModified": "2019-05-14T17:24:14.231+0000",
            "CodeSha256": "Rfwvx2l47yrOWtuRVJF/c/fOGuKUsb4IML8hCcmC3x8=",
            "Version": "$LATEST",
            "Environment": {
                "Variables": {}
            },
            "TracingConfig": {
                "Mode": "PassThrough"
            },
            "RevisionId": "10d77fff-fc9f-4e9a-8d66-77b4fd4a66e9"
        }
    ]
}
```

8. Set Environment Variables including DefaultConnection, Audience, Issuer, and SecretKey. DefaultConnection contains the connection string for connecting to your database. Audience, Issuer, and SecretKey are needed to issue JWT securely.
```
aws lambda update-function-configuration \
            --function-name ${FUNCTIONNAME} \
            --environment Variables={"DefaultConnection=${CONNECTIONSTRING},Audience=MyAudience,Issuer=MyIssuer,SecretKey=mysupersecret_secretkey!123"}
```

9. Create an AWS APIGateway API
```
aws apigateway create-rest-api --name saw --region ${REGION} --endpoint-configuration types=REGIONAL
```
Note the resulting API's id value __${APIID}__ in the response. You need it in the next step and later. 
```
{
    "name": "saw", 
    "id": "te6si5ach7", 
    "createdDate": 1557942249
}
```

10. Call the get-resources command to get the root resource id.
```
aws apigateway get-resources --rest-api-id ${APIID} --region {REGION}
```
Note the root resource id value __${PARENTRESOURCEID}__. You need it in the next step and later 
```
{
    "items": [
        {
            "path": "/", 
            "id": "krznpq9xpg"
        }
    ]
}
```

11. Call create-resource to create an API Gateway Resource.
```
aws apigateway create-resource \
      --rest-api-id ${APIID} \
      --region ${REGION} \
      --parent-id ${PARENTRESOURCEID} \
      --path-part {proxy+}
```
Note the resulting resource's id value __${RESOURCEID}__. You need it in the next step.
```
{
    "path": "/{proxy+}", 
    "pathPart": "{proxy+}", 
    "id": "2jf6xt", 
    "parentId": "krznpq9xpg"
}
```

12. Call put-method to create an ANY method request of ANY /{proxy+}
```
aws apigateway put-method \
       --rest-api-id ${APIID} \
       --region ${REGION} \
       --resource-id ${RESOURCEID} \
       --http-method ANY \
       --authorization-type "NONE" 
```

13. Call put-integration to set up the integration of the ANY /{proxy+} method with a Lambda function.
```
aws apigateway put-integration \
        --region ${REGION} \
        --rest-api-id ${APIID} \
        --resource-id ${RESOURCEID} \
        --http-method ANY \
        --type AWS_PROXY \
        --integration-http-method POST \
        --uri arn:aws:apigateway:${REGION}:lambda:path/2015-03-31/functions/${FUNCTIONARN}/invocations 
```

14. Call add-permission to give API Gateway permission to invoke your Lambda function.
```
aws lambda add-permission \
        --function-name ${FUNCTIONARN} \
        --action "lambda:InvokeFunction" \
        --statement-id 1 \
        --principal apigateway.amazonaws.com \
        --source-arn "arn:aws:execute-api:"${REGION}":"${ACCOUNT_ID}":"${API_ID}"/*/*/*"
```

15. Call create-deployment to deploy the API to a test stage. Note stage name __${STAGENAME}__.
```
aws apigateway create-deployment --rest-api-id ${APIID} --stage-name test
```

16. The API is now invoked by submitting requests the URL.
```
https://${APIID}.execute-api.${REGION}.amazonaws.com/${STAGENAME}/blog
```

17. First register a user.
```
POST https://${APIID}.execute-api.${REGION}.amazonaws.com/${STAGENAME}/blog/account/register
Body
application/json
{"Email" : "YourName@ok.com", "PasswordHash" : "Abc!123"}
```

18. Now login the user to get a security token.
```
POST https://${APIID}.execute-api.${REGION}.amazonaws.com/${STAGENAME}/blog/account/login
Body
application/json
{"Email" : "YourName@ok.com", "PasswordHash" : "Abc!123"}
```

19. Use your security tokens to send JSON GET, POST, PUT, and DELETE requests.
```
GET https://${APIID}.execute-api.${REGION}.amazonaws.com/${STAGENAME}/blog/articles
Headers
Authorization Bearer eyJhbGc...FULL TOKEN...RrXfOA
{
"ArticleId": 1, 
"ArticleTitle": "How to Floss", 
"ArticleText": "Stand with your knees slightly bent and swing your arms to the left..." 
}, 
{ 
"ArticleId": 2, 
"ArticleTitle": "How to Best Mates", 
"ArticleText": "Stretch arms out and bend elbow leaving fingers pointing downwards..." 
}, 
{ 
"ArticleId": 3, 
"ArticleTitle": "How to Shoot", 
"ArticleText": "Jump on your left leg, swing your right leg back and forth..." 
}
```
```
GET https://${APIID}.execute-api.${REGION}.amazonaws.com/${STAGENAME}/blog/article/2 
Headers
Authorization Bearer eyJhbGc...FULL TOKEN...RrXfOA
{ 
"ArticleId": 2, 
"ArticleTitle": "How to Best Mates", 
"ArticleText": "Stretch arms out and bend elbow leaving fingers pointing downwards..." 
} 
```

```
POST https://${APIID}.execute-api.${REGION}.amazonaws.com/${STAGENAME}/blog/article 
{"ArticleId":"4",ArticleTitle":"How to Running Man","ArticleText":"Lift your right foot and slide left foot back..."} 
```

```
PUT https://${APIID}.execute-api.${REGION}.amazonaws.com/${STAGENAME}/blog/article/2 
{"ArticleId":"2","ArticleTitle":"How to Moonwalk","ArticleText":"Place one foot directly..."} 
```

```
DELETE https://${APIID}.execute-api.${REGION}.amazonaws.com/${STAGENAME}/blog/article/3
```


### CREDITS ###

This project borrows from the following open source projects on Github. Thank You!

| user | repo |
| ------------- | ------------- |
| [benfoster](https://github.com/benfoster) | [BareMetalApi](https://github.com/benfoster/BareMetalApi) |
| [aspnet](https://github.com/aspnet) | [ToDoApi](https://github.com/aspnet/AspNetCore.Docs/tree/master/aspnetcore/mobile/native-mobile-backend/sample/ToDoApi) |
| [aws](https://github.com/aws) | [aws-lambda-dotnet](https://github.com/aws/aws-lambda-dotnet/tree/master/Blueprints/BlueprintDefinitions/Msbuild-NETCore_2_1/AspNetCoreWebAPI/template/src/BlueprintBaseName.1) |
| [Longfld](https://github.com/Longfld) | [ASPNETcoreAngularJWT](https://github.com/Longfld/ASPNETcoreAngularJWT) |
