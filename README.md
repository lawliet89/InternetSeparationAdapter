# InternetSeparationAdapter ![Build Status](https://api.travis-ci.org/lawliet89/InternetSeparationAdapter.svg)

A utility to fetch email messages from Gmail and send it to Telegram via a bot.

## Requirements
 - [Mono 4](http://www.mono-project.com/download/)
 - [Gmail API Key](https://developers.google.com/gmail/api/guides/) with scopes `GmailReadonly` and `GmailModify`
 - [Telegram Bot API Key](https://core.telegram.org/bots/api)

## Configuration
A configuration file should be in `JSON` format. The format should adhere to the `Config` class in the source code.

### Example
```json
{
  "TelegramApiToken": "token from @BotFather",
  "TelegramChatGroupIds": [-11111111111111],
  "StoredGoogleCredentialsPath": "./config/somewhere",
  "GoogleCredentials": {}
}
```
The `GoogleCredentials` object in the JSON file should be copied from the OAuth credentials provided by Google when
you create the credentials to access the Gmail API.

## Running it
Build the application using mono:

```bash
nuget restore
xbuild /property:Configuration=Release /property:OutDir=build/
build/InternetSeparationAdapter.exe path/to/config.json
```

There is also a Docker image to run the application, but you need to have gotten the OAuth credentials for the user
you are trying to connect to before hand. You can configure `StoredGoogleCredentialsPath` above to point it to
somewhere where you can then dump the credentials. See
[the issue](https://github.com/lawliet89/InternetSeparationAdapter/issues/3).

Since the Docker image mounts `/usr/src/app/build/config` as a volume, you can put it inside your `config` directory.

An example `docker-compose.yml` file might look like:

```yml
version: "2"
services:
  xxx:
    build: .
    restart: always
    volumes:
      - ./config:
    command: ["config/xxx.json"]

```

## Tests
Tests are missing! We need to write them =X
