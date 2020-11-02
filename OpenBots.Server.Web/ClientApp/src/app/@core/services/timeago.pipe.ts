import { Pipe, PipeTransform } from '@angular/core';
import * as moment from "moment";
@Pipe({
  name: 'timeago'
})
export class TimeagoPipe implements PipeTransform {


  transform(dateUtc: string, args?: any): any {

    let localTime = moment
      .utc(dateUtc)
      .local()
      .toLocaleString();


    let nowTime = moment(localTime).fromNow();

    let currentTime = moment()
      .local()
      .toLocaleString();
    let elapsedTime = moment(currentTime).diff(localTime, "hours");

    if (elapsedTime >= 24) {
      var locale = window.navigator.language;
      moment.locale(locale);
      nowTime = `${moment(localTime).format("l LT")} `;
    }
    return nowTime;
  }
}
