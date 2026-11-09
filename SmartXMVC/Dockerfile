FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY SmartX.Shared/ SmartX.Shared/
COPY SmartXMVC/ SmartXMVC/
RUN dotnet publish SmartXMVC/SmartXMVC.csproj -c Release -o /out
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /out .
ENV ASPNETCORE_URLS=http://0.0.0.0:5080 SmartX__AllowHttp=true
EXPOSE 5080
ENTRYPOINT ["dotnet", "SmartXMVC.dll"]
