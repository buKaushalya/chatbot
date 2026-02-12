# RagApi

A .NET 8 Web API for Retrieval-Augmented Generation (RAG) with document ingestion, embedding, and chat capabilities. Uses ONNX for embeddings, PostgreSQL for storage, and supports integration with Ollama LLMs.

## Features
- **Document Ingestion:** Upload and chunk text documents into collections.
- **Embeddings:** Generate vector embeddings using ONNX models.
- **Semantic Search:** Search documents using vector similarity.
- **Chat API:** Chat endpoint powered by Ollama LLMs, with context retrieval from ingested documents.
- **PostgreSQL Storage:** Stores collections, documents, and embeddings.

## Project Structure
- `Api/Endpoints/` — HTTP endpoint definitions
- `Api/Workloads/` — Endpoint logic/handlers
- `Core/` — Options and record types
- `Database/` — DB connection and repositories
- `Services/` — Embedding, ingestion, chat, and Ollama integration
- `models/embedding/` — ONNX model and tokenizer

## Requirements
- .NET 8 SDK
- Docker (for PostgreSQL)
- ONNX Runtime
- Ollama LLM server (optional, for chat)


# RagApi

A .NET 8 Web API for Retrieval-Augmented Generation (RAG) with document ingestion, embedding, and chat capabilities. Uses ONNX for embeddings, PostgreSQL for storage, and supports integration with Ollama LLMs.

## Features
- **Document Ingestion:** Upload and chunk text documents into collections.
- **Embeddings:** Generate vector embeddings using ONNX models.
- **Semantic Search:** Search documents using vector similarity.
- **Chat API:** Chat endpoint powered by Ollama LLMs, with context retrieval from ingested documents.
- **PostgreSQL Storage:** Stores collections, documents, and embeddings.

## Project Structure
- `Api/Endpoints/` — HTTP endpoint definitions
- `Api/Workloads/` — Endpoint logic/handlers
- `Core/` — Options and record types
- `Database/` — DB connection and repositories
- `Services/` — Embedding, ingestion, chat, and Ollama integration
- `models/embedding/` — ONNX model and tokenizer

## Requirements
- .NET 8 SDK
- Docker (for PostgreSQL)
- ONNX Runtime
- Ollama LLM server (optional, for chat)

## Getting Started
1. **Clone the repo**
2. **Start PostgreSQL:**
   ```sh
   cd DocumentAgent/postgres
   docker-compose up -d
   ```
3. **Create the database schema:**
   - Run the DB initialization script after the database is up:
     ```sh
     psql -h localhost -U rag -d ragdb -f ../DBscript/RagDBscript
     ```
     (Script location: `RagApi/DBscript/RagDBscript`)
4. **Configure settings:**
   - Edit `RagApi/appsettings.json` for DB, embedding, and Ollama settings.
5. **Run the API:**
   ```sh
   dotnet run --project RagApi/RagApi
   ```
6. **API Endpoints:**
   - `POST /v1/collections` — Create a collection
   - `POST /v1/collections/{collectionId}/documents/text` — Upload text document
   - `POST /v1/collections/{collectionId}/search` — Search documents
   - `POST /v1/chat` — Chat with context

## Configuration
See `appsettings.json` for:
- Database connection string
- Embedding model/tokenizer paths
- Ollama LLM base URL and model

## License
MIT
