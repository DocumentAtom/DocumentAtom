# iWork Type Detection

Apple iWork files are .zip archives containing a directory structure and several .iwa files.  For a comprehensive overview of the file format, please see https://github.com/obriensp/iWorkFileFormat.

Discerning the type of iWork file from this structure is difficult as the file structures generally look very similar.

The mechanism used by DocumentAtom is as follows:

1) Determine if the file is a .ZIP archive.  If yes, unpack the archive.
2) Determine if the directory structure contains the subdirectories `Data`, `Index`, and `Metadata`.
3) Determine if the archive is a Keynote presentation by the existence of `Index/Slide*.iwa` files.
4) Determine if the archive is a Numbers spreadsheet by the existence of `Index/AnnotationAuthorStorage-*.iwa` and `Index/CalculationEngine-*.iwa` files.
5) Determine if the archive is a Pages document by the existence of `Index/AnnotationAuthorStorage.iwa` and `Index/CalculationEngine.iwa` files.

It appears through a cursory examination that Keynote's structure is distinct due to the presence of `Slide*.iwa` files in the `Index` subdirectory.  Numbers and Pages artifacts look very similar, however, it appears Numbers has a `CalculationEngine.iwa` file per worksheet, whereas Pages has one for the entire document, and similarly, the same for `AnnotationAuthorStorage.iwa`.

