import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import TextWithCopy from "#/components/text-with-copy/TextWithCopy";

// Mock clipboard API
Object.assign(navigator, {
  clipboard: {
    writeText: jest.fn(() => Promise.resolve()),
  },
});

describe("TextWithCopy", () => {
  beforeEach(() => {
    jest.clearAllMocks();
    jest.useFakeTimers();
  });

  afterEach(() => {
    jest.runOnlyPendingTimers();
    jest.useRealTimers();
  });

  describe("Rendering", () => {
    it("should render text correctly", () => {
      render(<TextWithCopy text="Sample text to copy" />);

      expect(screen.getByText("Sample text to copy")).toBeInTheDocument();
    });

    it("should render copy button", () => {
      render(<TextWithCopy text="Sample text" />);

      const copyButton = screen.getByRole("button");
      expect(copyButton).toBeInTheDocument();
    });

    it("should render with custom className", () => {
      const { container } = render(
        <TextWithCopy text="Sample text" className="custom-class" />
      );

      const flexContainer = container.querySelector(".custom-class");
      expect(flexContainer).toBeInTheDocument();
    });
  });

  describe("Accessibility", () => {
    it("should have copy icon button", () => {
      render(<TextWithCopy text="Sample text" />);

      const copyButton = screen.getByRole("button");
      expect(copyButton).toBeInTheDocument();
    });
  });
});
