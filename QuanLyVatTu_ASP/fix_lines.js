const fs = require('fs');
const f = 'd:\\A\\QuanLyVatTu_ASP.NET\\QuanLyVatTu_ASP\\Views\\SanPham\\ProductDetail.cshtml';
const lines = fs.readFileSync(f, 'utf8').split('\n');
console.log('Total lines:', lines.length);
// Remove lines 907-953 (0-indexed: 906-952)
const keep = [...lines.slice(0, 906), ...lines.slice(953)];
console.log('New total:', keep.length);
fs.writeFileSync(f, keep.join('\n'), 'utf8');
console.log('Done!');
