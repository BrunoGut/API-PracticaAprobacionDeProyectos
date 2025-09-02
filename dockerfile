#build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

#copiamos todo el proyecto
COPY . .

#restauramos y publicamos
RUN dotnet restore
RUN dotnet publish -c Release -o /app

#runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app .

#inyectar puerto
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}

#exponemos el puerto
EXPOSE 8080

ENTRYPOINT ["dotnet", "PracticaAprobacionDeProyectos.dll"]