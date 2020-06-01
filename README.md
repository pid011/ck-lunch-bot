# ck-lunch-bot

청강대 점심 메뉴 봇

## Requirements

|        Name        |   Requirements    |
| :----------------: | :---------------: |
|  CKLunchBot.Core   | .NET Standard 2.1 |
| CKLunchBot.Twitter |   .NET Core 3.1   |

## Build and Run

```shell
git clone https://github.com/pid011/ck-lunch-bot.git
cd ck-lunch-bot
dotnet publish src/CKLunchBot.Twitter -c Release -o ../bot_publish
cd ../bot_publish
./CKLunchBot.Twitter
```

## Start Bot

첫 실행 시 `config.json`이 생성됩니다. `config.json`을 열어 token 값, tweet time 값을 수정해주세요.
다시 `./CKLunchBot.Twitter`를 실행하면 정상적으로 봇이 작동됩니다.
