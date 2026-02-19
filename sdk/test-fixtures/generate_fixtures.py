"""One-time script to generate binary test fixtures for DocumentAtom SDK test harnesses."""

import struct
import zipfile
import io
import os

OUTDIR = os.path.dirname(os.path.abspath(__file__))


def generate_png():
    """Generate a minimal 1x1 white pixel PNG."""
    # Minimal valid PNG: 1x1 white pixel
    def chunk(chunk_type, data):
        c = chunk_type + data
        # CRC32 over type+data
        import binascii
        crc = binascii.crc32(c) & 0xFFFFFFFF
        return struct.pack(">I", len(data)) + c + struct.pack(">I", crc)

    signature = b"\x89PNG\r\n\x1a\n"
    # IHDR: width=1, height=1, bit_depth=8, color_type=2 (RGB)
    ihdr_data = struct.pack(">IIBBBBB", 1, 1, 8, 2, 0, 0, 0)
    ihdr = chunk(b"IHDR", ihdr_data)
    # IDAT: raw image data (filter=0, R=255, G=255, B=255)
    import zlib
    raw = b"\x00\xff\xff\xff"  # filter byte + RGB
    compressed = zlib.compress(raw)
    idat = chunk(b"IDAT", compressed)
    iend = chunk(b"IEND", b"")

    path = os.path.join(OUTDIR, "sample.png")
    with open(path, "wb") as f:
        f.write(signature + ihdr + idat + iend)
    print(f"  Created {path} ({os.path.getsize(path)} bytes)")


def generate_jpg():
    """Generate a minimal 1x1 white pixel JPEG."""
    # Minimal valid JPEG: 1x1 white pixel
    # SOI + APP0 (JFIF) + DQT + SOF0 + DHT + SOS + compressed data + EOI
    data = bytes([
        0xFF, 0xD8,  # SOI
        0xFF, 0xE0,  # APP0
        0x00, 0x10,  # Length 16
        0x4A, 0x46, 0x49, 0x46, 0x00,  # JFIF\0
        0x01, 0x01,  # Version 1.1
        0x00,        # Aspect ratio units: none
        0x00, 0x01,  # X density 1
        0x00, 0x01,  # Y density 1
        0x00, 0x00,  # No thumbnail
        # DQT
        0xFF, 0xDB,
        0x00, 0x43,  # Length 67
        0x00,        # 8-bit, table 0
    ] + [1] * 64 + [  # All-ones quantization table
        # SOF0
        0xFF, 0xC0,
        0x00, 0x0B,  # Length 11
        0x08,        # 8-bit precision
        0x00, 0x01,  # Height 1
        0x00, 0x01,  # Width 1
        0x01,        # 1 component
        0x01,        # Component ID 1
        0x11,        # Sampling 1x1
        0x00,        # Quant table 0
        # DHT (DC table)
        0xFF, 0xC4,
        0x00, 0x1F,  # Length 31
        0x00,        # DC table 0
        0x00, 0x01, 0x05, 0x01, 0x01, 0x01, 0x01, 0x01,
        0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,
        0x08, 0x09, 0x0A, 0x0B,
        # SOS
        0xFF, 0xDA,
        0x00, 0x08,  # Length 8
        0x01,        # 1 component
        0x01,        # Component 1
        0x00,        # DC/AC table 0/0
        0x00, 0x3F,  # Spectral selection 0-63
        0x00,        # Successive approximation
        # Encoded data for white 1x1: DC coefficient = 1023 (white in JPEG)
        0xFB, 0xD2, 0x8A, 0x00,
        # EOI
        0xFF, 0xD9,
    ])
    path = os.path.join(OUTDIR, "sample.jpg")
    with open(path, "wb") as f:
        f.write(data)
    print(f"  Created {path} ({os.path.getsize(path)} bytes)")


def generate_pdf():
    """Generate a minimal valid PDF with text content."""
    content = b"""%PDF-1.4
1 0 obj
<< /Type /Catalog /Pages 2 0 R >>
endobj
2 0 obj
<< /Type /Pages /Kids [3 0 R] /Count 1 >>
endobj
3 0 obj
<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792]
   /Contents 4 0 R /Resources << /Font << /F1 5 0 R >> >> >>
endobj
4 0 obj
<< /Length 124 >>
stream
BT
/F1 12 Tf
100 700 Td
(DocumentAtom SDK Test Document) Tj
0 -20 Td
(This is a sample PDF for testing the DocumentAtom SDK.) Tj
ET
endstream
endobj
5 0 obj
<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>
endobj
xref
0 6
0000000000 65535 f
0000000009 00000 n
0000000058 00000 n
0000000115 00000 n
0000000266 00000 n
0000000442 00000 n
trailer
<< /Size 6 /Root 1 0 R >>
startxref
521
%%EOF
"""
    path = os.path.join(OUTDIR, "sample.pdf")
    with open(path, "wb") as f:
        f.write(content)
    print(f"  Created {path} ({os.path.getsize(path)} bytes)")


def generate_docx():
    """Generate a minimal valid DOCX file."""
    buf = io.BytesIO()
    with zipfile.ZipFile(buf, "w", zipfile.ZIP_DEFLATED) as zf:
        zf.writestr("[Content_Types].xml", """<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
  <Default Extension="xml" ContentType="application/xml"/>
  <Override PartName="/word/document.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml"/>
</Types>""")
        zf.writestr("_rels/.rels", """<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="word/document.xml"/>
</Relationships>""")
        zf.writestr("word/_rels/document.xml.rels", """<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"/>""")
        zf.writestr("word/document.xml", """<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<w:document xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main">
  <w:body>
    <w:p><w:r><w:t>DocumentAtom SDK Test Document</w:t></w:r></w:p>
    <w:p><w:r><w:t>This is a sample Word document for testing the DocumentAtom SDK. The SDK provides comprehensive document processing capabilities for various file formats.</w:t></w:r></w:p>
    <w:p><w:r><w:t>Key features include multi-format support, OCR integration, and batch processing.</w:t></w:r></w:p>
  </w:body>
</w:document>""")
    path = os.path.join(OUTDIR, "sample.docx")
    with open(path, "wb") as f:
        f.write(buf.getvalue())
    print(f"  Created {path} ({os.path.getsize(path)} bytes)")


def generate_xlsx():
    """Generate a minimal valid XLSX file."""
    buf = io.BytesIO()
    with zipfile.ZipFile(buf, "w", zipfile.ZIP_DEFLATED) as zf:
        zf.writestr("[Content_Types].xml", """<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
  <Default Extension="xml" ContentType="application/xml"/>
  <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
  <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
  <Override PartName="/xl/sharedStrings.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sharedStrings+xml"/>
</Types>""")
        zf.writestr("_rels/.rels", """<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
</Relationships>""")
        zf.writestr("xl/_rels/workbook.xml.rels", """<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
  <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/sharedStrings" Target="sharedStrings.xml"/>
</Relationships>""")
        zf.writestr("xl/workbook.xml", """<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"
          xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
  <sheets><sheet name="Sheet1" sheetId="1" r:id="rId1"/></sheets>
</workbook>""")
        zf.writestr("xl/sharedStrings.xml", """<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<sst xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" count="6" uniqueCount="6">
  <si><t>Name</t></si>
  <si><t>Department</t></si>
  <si><t>Salary</t></si>
  <si><t>John Doe</t></si>
  <si><t>Engineering</t></si>
  <si><t>Jane Smith</t></si>
</sst>""")
        zf.writestr("xl/worksheets/sheet1.xml", """<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
  <sheetData>
    <row r="1">
      <c r="A1" t="s"><v>0</v></c>
      <c r="B1" t="s"><v>1</v></c>
      <c r="C1" t="s"><v>2</v></c>
    </row>
    <row r="2">
      <c r="A2" t="s"><v>3</v></c>
      <c r="B2" t="s"><v>4</v></c>
      <c r="C2"><v>75000</v></c>
    </row>
    <row r="3">
      <c r="A3" t="s"><v>5</v></c>
      <c r="B3" t="s"><v>4</v></c>
      <c r="C3"><v>80000</v></c>
    </row>
  </sheetData>
</worksheet>""")
    path = os.path.join(OUTDIR, "sample.xlsx")
    with open(path, "wb") as f:
        f.write(buf.getvalue())
    print(f"  Created {path} ({os.path.getsize(path)} bytes)")


def generate_pptx():
    """Generate a minimal valid PPTX file."""
    buf = io.BytesIO()
    with zipfile.ZipFile(buf, "w", zipfile.ZIP_DEFLATED) as zf:
        zf.writestr("[Content_Types].xml", """<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
  <Default Extension="xml" ContentType="application/xml"/>
  <Override PartName="/ppt/presentation.xml" ContentType="application/vnd.openxmlformats-officedocument.presentationml.presentation.main+xml"/>
  <Override PartName="/ppt/slides/slide1.xml" ContentType="application/vnd.openxmlformats-officedocument.presentationml.slide+xml"/>
</Types>""")
        zf.writestr("_rels/.rels", """<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="ppt/presentation.xml"/>
</Relationships>""")
        zf.writestr("ppt/_rels/presentation.xml.rels", """<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/slide" Target="slides/slide1.xml"/>
</Relationships>""")
        zf.writestr("ppt/presentation.xml", """<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<p:presentation xmlns:p="http://schemas.openxmlformats.org/presentationml/2006/main"
                xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
  <p:sldIdLst><p:sldId id="256" r:id="rId1"/></p:sldIdLst>
</p:presentation>""")
        zf.writestr("ppt/slides/slide1.xml", """<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<p:sld xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main"
       xmlns:p="http://schemas.openxmlformats.org/presentationml/2006/main"
       xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
  <p:cSld>
    <p:spTree>
      <p:nvGrpSpPr><p:cNvPr id="1" name=""/><p:cNvGrpSpPr/><p:nvPr/></p:nvGrpSpPr>
      <p:grpSpPr/>
      <p:sp>
        <p:nvSpPr><p:cNvPr id="2" name="Title"/><p:cNvSpPr/><p:nvPr/></p:nvSpPr>
        <p:spPr/>
        <p:txBody>
          <a:bodyPr/>
          <a:p><a:r><a:t>DocumentAtom SDK Test Presentation</a:t></a:r></a:p>
          <a:p><a:r><a:t>This is a sample PowerPoint for testing the DocumentAtom SDK.</a:t></a:r></a:p>
        </p:txBody>
      </p:sp>
    </p:spTree>
  </p:cSld>
</p:sld>""")
    path = os.path.join(OUTDIR, "sample.pptx")
    with open(path, "wb") as f:
        f.write(buf.getvalue())
    print(f"  Created {path} ({os.path.getsize(path)} bytes)")


if __name__ == "__main__":
    print("Generating binary test fixtures...")
    generate_png()
    generate_jpg()
    generate_pdf()
    generate_docx()
    generate_xlsx()
    generate_pptx()
    print("Done!")
