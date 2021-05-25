import { Pipe, PipeTransform } from '@angular/core';
import * as moment from 'moment';
const momentConstructor = moment;
@Pipe({
  name: 'timeZone',
})
export class TimeZonePipe implements PipeTransform {
  transform(value: unknown, ...args: unknown[]): unknown {
    if (!value) return '';
    // console.log('pipe', momentConstructor.utc(value).utcOffset());
    // console.log('pipe',moment.tz(''))
    // console.log('moment', momentConstructor.);
    return momentConstructor.utc(value).format();
  }
}
