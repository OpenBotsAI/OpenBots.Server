import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'orderBy',
  pure: true,
})
export class OrderByPipe implements PipeTransform {
  transform(value: any[], propertyName: string): any[] {
    console.log(propertyName);
    if (propertyName)
      return value.sort((a: any, b: any) =>
        // b[propertyName].localeCompare(a[propertyName])
        a[propertyName].localeCompare(b[propertyName])
      );
    else return value;
  }
}
