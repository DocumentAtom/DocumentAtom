from typing import List, Optional

from pydantic import BaseModel, ConfigDict, Field


class ChunkingConfigurationModel(BaseModel):
    """
    Represents chunking configuration settings.
    """

    enable: Optional[bool] = Field(default=None, alias="Enable")
    strategy: Optional[str] = Field(default=None, alias="Strategy")
    fixed_token_count: Optional[int] = Field(default=None, alias="FixedTokenCount")
    overlap_count: Optional[int] = Field(default=None, alias="OverlapCount")
    overlap_percentage: Optional[float] = Field(default=None, alias="OverlapPercentage")
    overlap_strategy: Optional[str] = Field(default=None, alias="OverlapStrategy")
    row_group_size: Optional[int] = Field(default=None, alias="RowGroupSize")
    context_prefix: Optional[str] = Field(default=None, alias="ContextPrefix")
    regex_pattern: Optional[str] = Field(default=None, alias="RegexPattern")
    model_config = ConfigDict(populate_by_name=True)


class ApiProcessorSettingsModel(BaseModel):
    """
    Represents API processor settings for atom extraction.
    """

    trim_text: Optional[bool] = Field(default=None, alias="TrimText")
    remove_binary_from_text: Optional[bool] = Field(default=None, alias="RemoveBinaryFromText")
    extract_atoms_from_images: Optional[bool] = Field(default=None, alias="ExtractAtomsFromImages")
    chunking: Optional[ChunkingConfigurationModel] = Field(default=None, alias="Chunking")
    column_delimiter: Optional[str] = Field(default=None, alias="ColumnDelimiter")
    has_header_row: Optional[bool] = Field(default=None, alias="HasHeaderRow")
    rows_per_atom: Optional[int] = Field(default=None, alias="RowsPerAtom")
    build_hierarchy: Optional[bool] = Field(default=None, alias="BuildHierarchy")
    extract_metadata: Optional[bool] = Field(default=None, alias="ExtractMetadata")
    max_depth: Optional[int] = Field(default=None, alias="MaxDepth")
    property_handling: Optional[str] = Field(default=None, alias="PropertyHandling")
    preserve_original_structure: Optional[bool] = Field(default=None, alias="PreserveOriginalStructure")
    root_element_name: Optional[str] = Field(default=None, alias="RootElementName")
    include_attributes: Optional[bool] = Field(default=None, alias="IncludeAttributes")
    include_comments: Optional[bool] = Field(default=None, alias="IncludeComments")
    sheet_names: Optional[List[str]] = Field(default=None, alias="SheetNames")
    include_formulas: Optional[bool] = Field(default=None, alias="IncludeFormulas")
    include_hidden_sheets: Optional[bool] = Field(default=None, alias="IncludeHiddenSheets")
    model_config = ConfigDict(populate_by_name=True)


class AtomRequestModel(BaseModel):
    """
    Represents an atom extraction request with settings and base64-encoded data.
    """

    settings: Optional[ApiProcessorSettingsModel] = Field(default=None, alias="Settings")
    data: str = Field(alias="Data")
    model_config = ConfigDict(populate_by_name=True)
