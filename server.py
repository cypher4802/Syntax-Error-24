import socket
import json
import threading
import time
import signal
import sys
import logging

# Configure logging
logging.basicConfig(
    level=logging.DEBUG,
    format='%(asctime)s - %(levelname)s - %(message)s'
)

class PoseServer:
    def __init__(self, host='localhost', port=12345, refresh_rate=0.5):  # Corrected here
        logging.info("Initializing PoseServer...")
        self.host = host
        self.port = port
        self.server_socket = None
        self.clients = []
        self.running = False
        self.lock = threading.Lock()
        self.refresh_rate = refresh_rate
        self.last_process_time = {}
        
        # Setup signal handlers
        signal.signal(signal.SIGINT, self.signal_handler)
        signal.signal(signal.SIGTERM, self.signal_handler)
        logging.info(f"Server configured with refresh rate: {refresh_rate} seconds")

    def signal_handler(self, signum, frame):
        logging.info("\nShutdown signal received...")
        self.stop()
        sys.exit(0)

    def process_pose_data(self, pose_data, client_id):
        current_time = time.time()
        
        if client_id in self.last_process_time:
            time_since_last_process = current_time - self.last_process_time[client_id]
            if time_since_last_process < self.refresh_rate:
                return
        
        self.last_process_time[client_id] = current_time
        logging.info(f"Client {client_id} - Received pose data: {pose_data}")
        
        # Print specific pose information
        if isinstance(pose_data, dict):
            vertical_status = pose_data.get('vertical_status', 'unknown')
            left_stage = pose_data.get('left_stage', 'unknown')
            right_stage = pose_data.get('right_stage', 'unknown')
            kick_type = pose_data.get('kick_type', 'unknown')
            
            print("\nPose Status Update:")
            print(f"Client ID: {client_id}")
            print(f"Vertical Position: {vertical_status}")
            print(f"Left Arm Stage: {left_stage}")
            print(f"Right Arm Stage: {right_stage}")
            print(f"Kick Type: {kick_type}")
            print(f"Current Time: {current_time}")
            print(f"Last Process Time: {self.last_process_time[client_id]}")
            print(f"Refresh Rate: {self.refresh_rate} seconds")
            print("-" * 40)
        
        return pose_data

    def handle_client(self, client_socket, address):
        client_id = f"{address[0]}:{address[1]}"
        logging.info(f"Starting handler for client {client_id}")
        buffer = ""
        
        try:
            while self.running:
                client_socket.settimeout(1.0)
                try:
                    data = client_socket.recv(1024)
                    if not data:
                        logging.info(f"No data received from client {client_id}")
                        break
                    
                    print(f"Raw data received from client {client_id}: {data.decode()}")
                    buffer += data.decode()
                    
                    while '\n' in buffer:
                        line, buffer = buffer.split('\n', 1)
                        try:
                            pose_data = json.loads(line)
                            processed_data = self.process_pose_data(pose_data, client_id)
                            
                            if processed_data:
                                response = json.dumps(processed_data) + '\n'
                                client_socket.send(response.encode())
                                
                        except json.JSONDecodeError as e:
                            logging.error(f"JSON decode error for client {client_id}: {e}")
                            print(f"Problem data: {line}")
                    
                except socket.timeout:
                    continue
                except Exception as e:
                    logging.error(f"Error handling client {client_id}: {e}")
                    break
                    
                time.sleep(0.01)
                    
        except Exception as e:
            logging.error(f"Handler error for client {client_id}: {e}")
        finally:
            with self.lock:
                if client_socket in self.clients:
                    self.clients.remove(client_socket)
                if client_id in self.last_process_time:
                    del self.last_process_time[client_id]
            client_socket.close()
            logging.info(f"Client disconnected: {client_id}")

    def start(self):
        print("\n" + "="*50)
        print("Starting Pose Detection Server")
        print("="*50)
        
        try:
            self.server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            self.server_socket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
            self.server_socket.bind((self.host, self.port))
            self.server_socket.listen(5)
            self.running = True
            
            print(f"\nServer is listening on {self.host}:{self.port}")
            print(f"Refresh rate: {self.refresh_rate} seconds")
            print("\nWaiting for connections...")
            print("-"*50 + "\n")
            
            while self.running:
                try:
                    self.server_socket.settimeout(1.0)
                    client_socket, address = self.server_socket.accept()
                    print(f"\n>> New client connected from {address[0]}:{address[1]}")
                    
                    with self.lock:
                        self.clients.append(client_socket)
                    
                    client_thread = threading.Thread(
                        target=self.handle_client,
                        args=(client_socket, address)
                    )
                    client_thread.daemon = True
                    client_thread.start()
                    
                except socket.timeout:
                    continue
                except Exception as e:
                    if self.running:
                        logging.error(f"Connection error: {e}")
                    
        except Exception as e:
            logging.error(f"Server error: {e}")
        finally:
            self.stop()

    def stop(self):
        logging.info("Stopping server...")
        self.running = False
        
        with self.lock:
            for client in self.clients:
                try:
                    client.close()
                except:
                    pass
            self.clients.clear()
        
        if self.server_socket:
            try:
                self.server_socket.close()
            except:
                pass
            
        print("\nServer stopped successfully")
        print("="*50)

def main():
    print("\nPose Detection Server - Debug Mode")
    print("Press Ctrl+C to stop the server")
    print("-"*50)
    
    server = PoseServer(refresh_rate=0.5)
    try:
        server.start()
    except KeyboardInterrupt:
        print("\nKeyboard interrupt received...")
    except Exception as e:
        print(f"\nUnexpected error: {e}")
    finally:
        server.stop()
 
if __name__ == "__main__":
    main()
