from typing import Any, Dict, List, Optional

from pydantic import BaseModel, ConfigDict, Field


class ChunkModel(BaseModel):
    """
    Represents a chunk extracted from an atom.
    """

    position: int = Field(alias="Position")
    length: int = Field(alias="Length")
    md5_hash: Optional[str] = Field(default=None, alias="MD5Hash")
    sha1_hash: Optional[str] = Field(default=None, alias="SHA1Hash")
    sha256_hash: Optional[str] = Field(default=None, alias="SHA256Hash")
    text: Optional[str] = Field(default=None, alias="Text")
    model_config = ConfigDict(populate_by_name=True)


class AtomModel(BaseModel):
    """
    Represents an extracted atom.
    """

    parent_guid: Optional[str] = Field(default=None, alias="ParentGUID")
    guid: Optional[str] = Field(default=None, alias="GUID")
    type: Optional[str] = Field(default=None, alias="Type")
    sheet_name: Optional[str] = Field(default=None, alias="SheetName")
    cell_identifier: Optional[str] = Field(default=None, alias="CellIdentifier")
    page_number: Optional[int] = Field(default=None, alias="PageNumber")
    position: Optional[int] = Field(default=None, alias="Position")
    length: int = Field(default=0, alias="Length")
    rows: Optional[int] = Field(default=None, alias="Rows")
    columns: Optional[int] = Field(default=None, alias="Columns")
    title: Optional[str] = Field(default=None, alias="Title")
    subtitle: Optional[str] = Field(default=None, alias="Subtitle")
    md5_hash: Optional[str] = Field(default=None, alias="MD5Hash")
    sha1_hash: Optional[str] = Field(default=None, alias="SHA1Hash")
    sha256_hash: Optional[str] = Field(default=None, alias="SHA256Hash")
    header_level: Optional[int] = Field(default=None, alias="HeaderLevel")
    formatting: Optional[str] = Field(default=None, alias="Formatting")
    bounding_box: Optional[Dict[str, Any]] = Field(default=None, alias="BoundingBox")
    text: Optional[str] = Field(default=None, alias="Text")
    unordered_list: Optional[List[str]] = Field(default=None, alias="UnorderedList")
    ordered_list: Optional[List[str]] = Field(default=None, alias="OrderedList")
    table: Optional[Dict[str, Any]] = Field(default=None, alias="Table")
    binary: Optional[str] = Field(default=None, alias="Binary")
    quarks: Optional[List["AtomModel"]] = Field(default=None, alias="Quarks")
    chunks: Optional[List[ChunkModel]] = Field(default=None, alias="Chunks")
    model_config = ConfigDict(populate_by_name=True)
