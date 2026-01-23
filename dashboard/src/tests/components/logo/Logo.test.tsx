import { render, screen } from "@testing-library/react";
import DocuAtomLogo from "#/components/logo/Logo";

describe("DocuAtomLogo", () => {
  describe("Rendering", () => {
    it("should render logo image and text by default", () => {
      render(<DocuAtomLogo />);

      const logoImage = screen.getByAltText("Document Atom");
      expect(logoImage).toBeInTheDocument();
      expect(logoImage).toHaveAttribute(
        "src",
        expect.stringContaining("logo.png")
      );

      const logoText = screen.getByText("Document Atom");
      expect(logoText).toBeInTheDocument();
    });

    it("should render only logo image when showOnlyIcon is true", () => {
      render(<DocuAtomLogo showOnlyIcon={true} />);

      const logoImage = screen.getByAltText("Document Atom");
      expect(logoImage).toBeInTheDocument();

      const logoText = screen.queryByText("Document Atom");
      expect(logoText).not.toBeInTheDocument();
    });

    it("should render logo with custom image size", () => {
      render(<DocuAtomLogo imageSize={60} />);

      const logoImage = screen.getByAltText("Document Atom");
      expect(logoImage).toBeInTheDocument();
      expect(logoImage).toHaveAttribute("height", "60");
      expect(logoImage).toHaveAttribute("width", "60");
    });

    it("should render logo with custom text size", () => {
      render(<DocuAtomLogo size={24} />);

      const logoText = screen.getByText("Document Atom");
      expect(logoText).toBeInTheDocument();
      expect(logoText).toHaveStyle({ fontSize: "24px" });
    });

    it("should render logo with default sizes when no props provided", () => {
      render(<DocuAtomLogo />);

      const logoImage = screen.getByAltText("Document Atom");
      expect(logoImage).toHaveAttribute("height", "45");
      expect(logoImage).toHaveAttribute("width", "45");

      const logoText = screen.getByText("Document Atom");
      expect(logoText).toHaveStyle({ fontSize: "20px" });
    });

    it("should have correct structure with flex container", () => {
      const { container } = render(<DocuAtomLogo />);

      // Check if the logo is wrapped in a flex container
      const flexContainer = container.querySelector(".ant-flex");
      expect(flexContainer).toBeInTheDocument();
    });
  });

  describe("Props combinations", () => {
    it("should handle all props together", () => {
      render(<DocuAtomLogo showOnlyIcon={false} size={30} imageSize={50} />);

      const logoImage = screen.getByAltText("Document Atom");
      expect(logoImage).toHaveAttribute("height", "50");
      expect(logoImage).toHaveAttribute("width", "50");

      const logoText = screen.getByText("Document Atom");
      expect(logoText).toHaveStyle({ fontSize: "30px" });
    });

    it("should respect showOnlyIcon over other props", () => {
      render(<DocuAtomLogo showOnlyIcon={true} size={30} imageSize={50} />);

      const logoImage = screen.getByAltText("Document Atom");
      expect(logoImage).toBeInTheDocument();

      // Text should not render even with size prop
      const logoText = screen.queryByText("Document Atom");
      expect(logoText).not.toBeInTheDocument();
    });
  });
});
