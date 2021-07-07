import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'allFileSize',
})
export class AllFileSizePipe implements PipeTransform {
  private units = ['bytes', 'KB', 'MB', 'GB', 'TB', 'PB'];
  transform(bytes: number = 0, precision: number = 2): string {
    if (isNaN(parseFloat(String(bytes))) || !isFinite(bytes)) return '?';
    // const sizeInMB = 1024 * 1024;
    // below if condition is unsued code when user added storage drives
    // if (bytes < sizeInMB) {
    //   bytes = bytes / sizeInMB;
    //   return bytes.toFixed(+precision) + ' ' + this.units[2];
    // } else {
    let unit = 0;
    while (bytes >= 1024) {
      bytes /= 1024;
      unit++;
    }

    // return bytes.toFixed(+precision) + ' ' + this.units[unit];
    return bytes + ' ' + this.units[unit];
  }
}
