export async function pickDirectory() {
    try {
        const dirHandle = await window.showDirectoryPicker();
        let files = [];

        // Recursive function to walk folders
        async function readDir(handle, path) {
            for await (const [name, entry] of handle.entries()) {
                if (entry.kind === "file") {
                    files.push(path + name);
                } else if (entry.kind === "directory") {
                    await readDir(entry, path + name + "/");
                }
            }
        }

        await readDir(dirHandle, "");
        return files;
    } catch (err) {
        console.error("Directory picking cancelled or failed", err);
        return [];
    }
}