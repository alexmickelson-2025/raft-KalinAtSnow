FROM mcr.microsoft.com/dotnet/sdk:8.0
 
RUN apt-get update && \
    apt-get install -y curl && \
    rm -rf /var/lib/apt/lists/*
 
 
WORKDIR /app
 
COPY . /app

RUN dotnet restore raftapi/raftapi.csproj
 
CMD dotnet run --project raftapi