from typing import Any, Dict, Optional

from pydantic import BaseModel, ConfigDict, Field


class AtomModel(BaseModel):
    """
    Represents an extracted atom.
    """

    content: str = Field(alias="Content")
    atom_type: Optional[str] = Field(default=None, alias="AtomType")
    position: Optional[Dict[str, Any]] = Field(default=None, alias="Position")
    metadata: Optional[Dict[str, Any]] = Field(default=None, alias="Metadata")
    model_config = ConfigDict(populate_by_name=True)
