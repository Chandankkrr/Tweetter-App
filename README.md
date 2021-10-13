## Run Locally

Clone the project

```bash
  git clone https://github.com/Chandankkrr/Tweetter-App.git
```

Switch to the project directory

```bash
  cd Tweetter-App
```

Make sure to create a `local.settings.json` file with the following 

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated"
  }
}
```
Build

```bash
  dotnet build
```

Run - Ensure you have Azure functions runtime installed

```bash
  func start --verbose 
```

Update TweetterTrigger schedule as necessary

## License

[MIT](https://choosealicense.com/licenses/mit/)

  