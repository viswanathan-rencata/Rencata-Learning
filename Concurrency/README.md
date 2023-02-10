# Concurrency Training :rocket:

Core concepts:

* Concurrency: Doing more than one thing at a time
* Thread: is a path of execution within a process
* Multithreading: is a form of concurrency that uses multiple threads of execution
* Asynchrony: is a form of getting concurrency without multi-threading

There are 2 fundamental operation types:
- I/O-bound
- CPU-bound 

### Concurrency

Doing more than one thing at a time.

- Asynchronous programming
  - I/O-bound
- Multithreading programming
  - CPU-bound
  - Parallel processing

### Asynchronous Programming

Is a form of concurrency that uses futures or callbacks to avoid unnecessary threads.

AKA Future (or Promise): is a type that represents some operation that will complete in the future.

The main idea is to be thread-less:
- There is no thread in the background waiting to come back
- No threads are created
- No threads should be used

## Topics

In this training series, we'll review the following topics:

- Capture and Assess Syncrhonization Context
- Review and assess the task statuses
- Exception Handling Behavior when using Tasks
- Working with Multiple Concurrent Tasks
- Waiting for the First Task that completes
    - Introducing a Simple Cancellation Token
    - Introducing a Cancellation Via Timeout
- Capturing the Syncrhonization Context
- Analyging a Poorly written RetryAsync scenario