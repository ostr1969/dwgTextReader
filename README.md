# Blazor File Indexer -  [WebTails](https://github.com/ostr1969/dwgTextReader/tree/master/src/webtail)

This is a **Blazor Server** application that allows you to:

- Select folders on your system  
- Index **DWG**, **PDF**, and **text** files  
- Search the indexed files and see results with **highlighted words**  
- Manage a predefined list of words in `appsettings.json`  

The project uses [ACadSharp](https://github.com/DomCR/ACadSharp) for DWG file parsing.

---

## Requirements

- **.NET 8 SDK** (or later)  
- **Elasticsearch** running locally, with **no authentication**  
- **Google Chrome** (or another modern browser)  

⚠️ The app is a **Blazor Server** app. It is configured **without HTTPS** and **without authentication**. Use only in trusted environments.

---

## Configuration

1. **Modify `appsettings.json`**  
   - Update the `"Words"` section to adjust the predefined list of words used in searches.  

2. **Fonts and Parsing Config**  
   - The project includes a `References` folder containing:  
     - AutoCAD fonts  
     - CSV configuration files for parsing Hebrew fonts  

---

## Usage

1. Start Elasticsearch.  
2. Run the app.  
3. Open a browser and navigate to:  
4. Use the UI to:  
- Select folders to index  
- Wait for indexing to finish  
- Perform searches (results will show your search words highlighted)

---

## Screenshots / Usage Examples

> *(Replace the placeholders with real screenshots when available)*

### Selecting a folder to index
![Folder Selection Screenshot](src/webtail/doc/images/Index.png)


### Search interface with results
![Search Results Screenshot](src/webtail/doc/images/Search.png)



---

## Publishing

To build a self-contained release (no .NET runtime required on the target machine):

```bash
dotnet publish -c Release -r win-x64 --self-contained true -o "C:\MyApp\Publish"
