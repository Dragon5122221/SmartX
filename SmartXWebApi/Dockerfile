FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY SmartX.Shared/ SmartX.Shared/
COPY SmartX.Core/ SmartX.Core/
COPY SmartXWebApi/ SmartXWebApi/
RUN dotnet publish SmartXWebApi/SmartXWebApi.csproj -c Release -o /out
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /out .
ENV ASPNETCORE_URLS=http://0.0.0.0:5267 SmartX__AllowHttp=true
EXPOSE 5267
ENTRYPOINT ["dotnet", "SmartXWebApi.dll"]
