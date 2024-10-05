Auth doesn't work when:
swa start http://localhost:5000 --data-api-devserver-url http://localhost:7071 -api-location Api

It does however work for:
swa start
swa start http://localhost:5000 

"swa start"" does not allow auth on client routes, which is expected


# Auth
Live:
- For client, use [Authorize]
- For backend, use routes in staticwebapp.config.json
Local:
Everything works fine with: swa start http://localhost:5000 
But using: swa start http://localhost:5000 --data-api-devserver-url http://localhost:7071
will NOT protect functions/backend (this works as expected live)
HMM IT WORKS?

AUTH DO WORK, you are just doing it wrong.



1. Start Client/API from Visual Studio (multi project)
2. swa start http://localhost:5000  --api-devserver-url http://localhost:7071


TODO:
- How does the GH workflow token work? It seems empty.