import os
f = r'd:\A\QuanLyVatTu_ASP.NET\QuanLyVatTu_ASP\Views\SanPham\ProductDetail.cshtml'
with open(f, 'r', encoding='utf-8') as fp:
    lines = fp.readlines()
print(f"Original lines: {len(lines)}")
# Remove 0-indexed lines 906..950 (1-indexed 907..951)
keep = [l for i, l in enumerate(lines) if i < 906 or i > 950]
print(f"New lines: {len(keep)}")
with open(f, 'w', encoding='utf-8') as fp:
    fp.writelines(keep)
print("Done!")
