import React from "react";
import { useAppContext } from "#/hooks/appHooks";
import { ThemeEnum } from "#/types/types";

const SIZE = 20;

const iconLayerStyle = (visible: boolean, hiddenTransform: string): React.CSSProperties => ({
  position: "absolute",
  inset: 0,
  display: "inline-flex",
  transition: "opacity 0.3s ease, transform 0.3s ease",
  opacity: visible ? 1 : 0,
  transform: visible ? "rotate(0deg) scale(1)" : hiddenTransform,
});

const ThemeModeSwitch = () => {
  const { theme, setTheme } = useAppContext();
  const isDark = theme === ThemeEnum.DARK;

  const toggle = () =>
    setTheme(isDark ? ThemeEnum.LIGHT : ThemeEnum.DARK);

  return (
    <button
      type="button"
      aria-label={isDark ? "Switch to light mode" : "Switch to dark mode"}
      aria-pressed={isDark}
      onClick={toggle}
      style={{
        position: "relative",
        width: SIZE,
        height: SIZE,
        padding: 0,
        border: "none",
        background: "transparent",
        cursor: "pointer",
        color: "currentColor",
        lineHeight: 0,
      }}
    >
      {/* Sun */}
      <span style={iconLayerStyle(!isDark, "rotate(-90deg) scale(0.6)")}>
        <svg
          width={SIZE}
          height={SIZE}
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          strokeWidth={2}
          strokeLinecap="round"
          strokeLinejoin="round"
        >
          <circle cx="12" cy="12" r="5" />
          <line x1="12" y1="1" x2="12" y2="3" />
          <line x1="12" y1="21" x2="12" y2="23" />
          <line x1="4.22" y1="4.22" x2="5.64" y2="5.64" />
          <line x1="18.36" y1="18.36" x2="19.78" y2="19.78" />
          <line x1="1" y1="12" x2="3" y2="12" />
          <line x1="21" y1="12" x2="23" y2="12" />
          <line x1="4.22" y1="19.78" x2="5.64" y2="18.36" />
          <line x1="18.36" y1="5.64" x2="19.78" y2="4.22" />
        </svg>
      </span>

      {/* Moon */}
      <span style={iconLayerStyle(isDark, "rotate(90deg) scale(0.6)")}>
        <svg
          width={SIZE}
          height={SIZE}
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          strokeWidth={2}
          strokeLinecap="round"
          strokeLinejoin="round"
        >
          <path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z" />
        </svg>
      </span>
    </button>
  );
};

export default ThemeModeSwitch;
