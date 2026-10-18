import { describe, expect, it } from "vitest";

describe("test environment", () => {
  it("parses player markup into a DOM", () => {
    document.body.innerHTML =
      '<div id="movie_player" class="html5-video-player ad-showing"></div>';

    const player = document.querySelector("#movie_player");

    expect(player?.classList.contains("ad-showing")).toBe(true);
  });
});
