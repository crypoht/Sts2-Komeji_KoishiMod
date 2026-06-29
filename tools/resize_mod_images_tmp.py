from pathlib import Path
from PIL import Image, ImageOps


ROOT = Path(r"D:\UINya\Godot_v4.5.1-stable_mono_win64\komeiji-koishi\mods\Komeiji_Koishi\images")
MAX_SIDE = 1024
EXTS = {".png", ".jpg", ".jpeg", ".webp"}


def save_image(image: Image.Image, path: Path) -> None:
    ext = path.suffix.lower()
    if ext == ".png":
        image.save(path, optimize=True, compress_level=9)
    elif ext in {".jpg", ".jpeg"}:
        if image.mode in {"RGBA", "LA", "P"}:
            image = image.convert("RGB")
        image.save(path, quality=88, optimize=True, progressive=True)
    elif ext == ".webp":
        image.save(path, quality=88, method=6)


def main() -> None:
    files = [p for p in ROOT.rglob("*") if p.is_file() and p.suffix.lower() in EXTS]
    before = sum(p.stat().st_size for p in files)
    resized = 0
    recompressed = 0
    skipped = 0
    errors: list[str] = []

    for path in files:
        try:
            old_size = path.stat().st_size
            with Image.open(path) as opened:
                image = ImageOps.exif_transpose(opened)
                width, height = image.size
                largest = max(width, height)
                if largest > MAX_SIDE:
                    scale = MAX_SIDE / largest
                    new_size = (max(1, round(width * scale)), max(1, round(height * scale)))
                    image = image.resize(new_size, Image.Resampling.LANCZOS)
                    save_image(image, path)
                    resized += 1
                else:
                    save_image(image, path)
                    if path.stat().st_size < old_size:
                        recompressed += 1
                    else:
                        skipped += 1
        except Exception as exc:
            errors.append(f"{path}: {exc}")

    after_files = [p for p in ROOT.rglob("*") if p.is_file() and p.suffix.lower() in EXTS]
    after = sum(p.stat().st_size for p in after_files)

    print(f"root={ROOT}")
    print(f"files={len(files)} resized={resized} recompressed={recompressed} skipped={skipped} errors={len(errors)}")
    print(f"before_mb={before / 1024 / 1024:.2f}")
    print(f"after_mb={after / 1024 / 1024:.2f}")
    print(f"saved_mb={(before - after) / 1024 / 1024:.2f}")
    if errors:
        print("errors:")
        for error in errors:
            print(error)


if __name__ == "__main__":
    main()
