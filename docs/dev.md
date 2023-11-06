
## Package downloads

Packages can be dowloaded from Tetrifact as either a zip containing all files for a given package, or as separate files. Package downloading is coded to handle very large builds with many files over unstable network connections.

Packages are partitioned by project. This does carry an effeciency cost, as a build in one project will not be able to source files from another for partial downloads, but the overhead of managing files outside of projects is too great - files would need to be stored in some kind of global hash table, and this rapidly ends up looking like re-implementing Tetrifact's server hash storage table.

### Zip downloads

Fetching a build as a zip has a series of stages which allow for resumption at each stage. 

Zip downloading takes place in stages. Zip files are downloaded in chunks. Each chunk is downloaded with a temporary chunk name, then renamed to a chunk once download has succeeded. If a chunk fails to download, download has to resume from scratch for that chunk.

Once all chunks are downloaded, the chunks are assembled into a file with a temporary zip name. On completion, the temporary zip is renamed to the zip, and all chunks are deleted. Unzipping then commences to a temporary unpack directory. If the temporary unpack directory exists, it is deleted. Once unzipping completes, the temporary unpack directory is renamed to the final package directory, and the zip file can be deleted.

It is assumed that once a zip has been fully downloaded and unpacked, it has integrity.

### Separate files

Packages can also be assembled from single files 

