import sys
f = r'd:\A\QuanLyVatTu_ASP.NET\QuanLyVatTu_ASP\Views\SanPham\ProductDetail.cshtml'
with open(f, 'r', encoding='utf-8') as fp:
    lines = fp.readlines()
print(f"Total lines: {len(lines)}")
# Keep lines 1-906 (index 0-905) and lines 954+ (index 953+)
# Remove lines 907-953 (index 906-952)
keep = lines[:906] + lines[953:]
print(f"New total: {len(keep)}")
with open(f, 'w', encoding='utf-8') as fp:
    fp.writelines(keep)
print("Done!")
sys.exit(0)
