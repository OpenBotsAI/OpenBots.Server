import { Pipe, PipeTransform } from '@angular/core';
import * as moment from 'moment';
const momentConstructor = moment;
@Pipe({
  name: 'timeDate',
})
export class TimeDatePipe implements PipeTransform {
  transform(value: moment.MomentInput, ...args: any[]): string {
    if (!value) {
      return '';
    }
    return momentConstructor(value).format(args[0]);
  }
}
