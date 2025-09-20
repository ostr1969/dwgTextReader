4. Use the UI to:  
- Select folders to index  
- Wait for indexing to finish  
- Perform searches (results will show your search words highlighted)

---

## Screenshots / Usage Examples

> *(Replace the placeholders with real screenshots when available)*

### Selecting a folder to index
![Folder Selection Screenshot](docs/images/folder-selection.png)

### Search interface with results
![Search Results Screenshot](docs/images/search-results.png)

### Highlighted search terms in context
![Highlighted Text Screenshot](docs/images/highlighted-text.png)

---

## Publishing

To build a self-contained release (no .NET runtime required on the target machine):

```bash
dotnet publish -c Release -r win-x64 --self-contained true -o "C:\MyApp\Publish"
