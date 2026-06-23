FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore "PRN232.LMS.CourseService/PRN232.LMS.CourseService.csproj"
RUN dotnet publish "PRN232.LMS.CourseService/PRN232.LMS.CourseService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "PRN232.LMS.CourseService.dll"]
