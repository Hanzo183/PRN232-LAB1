FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore "PRN232.LMS.StudentService/PRN232.LMS.StudentService.csproj"
RUN dotnet publish "PRN232.LMS.StudentService/PRN232.LMS.StudentService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "PRN232.LMS.StudentService.dll"]
