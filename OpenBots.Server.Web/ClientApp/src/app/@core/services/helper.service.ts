import { HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { FormControl } from '@angular/forms';
import { FileSizePipe } from 'ngx-filesize';
import { Rule, Usage } from '../../interfaces/ipFencing';
import { ItemsPerPage } from '../../interfaces/itemsPerPage';
import { TimeDatePipe } from '../pipe';

@Injectable({
  providedIn: 'root',
})
export class HelperService {
  itemPerPage: ItemsPerPage[];
  pipe: TimeDatePipe;
  fileSize: FileSizePipe;
  constructor() {}

  noWhitespaceValidator(control: FormControl) {
    const isWhitespace = (control.value || '').trim().length === 0;
    const isValid = !isWhitespace;
    return isValid ? null : { whitespace: true };
  }

  getItemsPerPage(): ItemsPerPage[] {
    return (this.itemPerPage = [
      { id: 5, name: '5 per page' },
      { id: 10, name: '10 per page' },
      { id: 25, name: '25 per page' },
      { id: 50, name: '50 per page' },
      { id: 100, name: '100 per page' },
    ]);
  }

  transformDate(value, format: string) {
    this.pipe = new TimeDatePipe();
    return this.pipe.transform(value, `${format}`);
  }

  changeBoolean(value: boolean | string): string {
    if (value) return 'Yes';
    else return 'No';
  }

  getETagHeaders(etag: string) {
    const headers = new HttpHeaders({ 'If-Match': etag });
    return headers;
  }

  getUsage(): Usage[] {
    return [
      { name: 'Allow', value: 1 },
      { name: 'Deny', value: -1 },
    ];
  }

  getRules(): Rule[] {
    return [
      { name: 'IPv4', value: 1 },
      { name: 'IPv4Range', value: 2 },
      { name: 'IPv6', value: 3 },
      { name: 'IPv6Range', value: 4 },
      { name: 'HTTP Header', value: 5 },
    ];
  }

  getFileSize(param: number): string | string[] {
    this.fileSize = new FileSizePipe();
    return this.fileSize.transform(param);
  }
}
