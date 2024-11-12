import os
from multiprocessing import shared_memory

SHARED_MEM_NAME = "ARFightingGameSharedMemory"  # The name of the shared memory segment
SHARED_MEM_SIZE = 256  # The expected size of the shared memory

def main():
    # Access the existing shared memory
    try:
        existing_shm = shared_memory.SharedMemory(name=SHARED_MEM_NAME)
    except FileNotFoundError:
        print(f"Shared memory named '{SHARED_MEM_NAME}' does not exist.")
        return

    # Read the content from shared memory
    action_bytes = existing_shm.buf[:SHARED_MEM_SIZE]  # Read the full size
    action_string = action_bytes.tobytes().decode('utf-8').rstrip()  # Decode and strip padding

    # Print the stored action
    print(f"Data read from shared memory: '{action_string}'")

    # Clean up
    existing_shm.close()

if __name__ == "__main__":
    main()
        