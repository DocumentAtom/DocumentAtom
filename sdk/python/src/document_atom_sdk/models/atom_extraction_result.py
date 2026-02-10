from typing import Any, Dict, List, Optional

from pydantic import BaseModel, ConfigDict, Field

from .atom import AtomModel


class AtomExtractionResultModel(BaseModel):
    """
    Result from atom extraction API.
    """

    atoms: List[AtomModel] = Field(default_factory=list, alias="Atoms")
    metadata: Optional[Dict[str, Any]] = Field(default=None, alias="Metadata")
    file_type: Optional[str] = Field(default=None, alias="FileType")
    model_config = ConfigDict(populate_by_name=True)
