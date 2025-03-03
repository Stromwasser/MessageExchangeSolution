# Message Exchange Solution 🚀  

## 🔹 How to Run the Project?  

### 🛠 1. Install Docker  
If you don’t have Docker installed, download and install it:  
🔗 [Get Docker](https://www.docker.com/get-started)  

### 📥 2. Clone the Repository and Start the System  
Run the following commands in your terminal:  

```sh
git clone https://github.com/Stromwasser/MessageExchangeSolution.git
cd MessageExchangeSolution
docker-compose up -d

```

### ✅ 3. Check if the Project is Running
After running the system, open the following links in your browser:

🔹 Swagger API Documentation:
👉 http://localhost:7043/swagger

🔹 Client Application:
👉 http://localhost:7080



### 🔍 4. How to Check Logs?
📌 API Logs
The API logs are saved inside the container in dated log files (Logs/api-log-YYYYMMDD.txt).

To check the latest log file name, use:


```sh
docker exec -it messageexchange_api ls -t /app/Logs/ | head -n 1

```
If you want real-time API logs, use:
```sh
docker logs -f messageexchange_api
```
### 🔄 5. Stopping and Restarting the System
To stop the system:
```sh
docker-compose down
```
To restart it:
```sh
docker-compose up -d
```
