import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'chevron',
})
export class ChevronPipe implements PipeTransform {
  transform(value: unknown[], totalCount): unknown {
    if (value && totalCount) {
      return value && value.length > 1 && totalCount > 5
        ? 'my-icon fa fa-chevron-up'
        : null;
    }
    return null;
  }
}
