# wxcloudrun-dotnet

[![GitHub license](https://img.shields.io/github/license/WeixinCloud/wxcloudrun-dotnet)](https://github.com/WeixinCloud/wxcloudrun-dotnet)

## 项目结构说明

```
.
├── Dockerfile
├── README.md
├── container.config.json
├── aspnetapp
```

- `aspnetapp`：项目入口，实现主要的读写 API
- `container.config.json`：模板部署「服务设置」初始化配置（二开请忽略）
- `Dockerfile`：容器配置文件

## 使用注意
如果不是通过微信云托管控制台部署模板代码，而是自行复制/下载模板代码后，手动新建一个服务并部署，需要在「服务设置」中补全以下环境变量，才可正常使用，否则会引发无法连接数据库，进而导致部署失败。
- MYSQL_ADDRESS
- MYSQL_PASSWORD
- MYSQL_USERNAME
以上三个变量的值请按实际情况填写。如果使用云托管内MySQL，可以在控制台MySQL页面获取相关信息。


## License

[MIT](./LICENSE)
