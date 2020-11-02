import {
  Directive,
  ElementRef,
  HostListener,
  Input,
  OnInit,
} from '@angular/core';

@Directive({
  selector: '[ngxSpecialChar]',
})
export class PreventSpecialCharDirective implements OnInit {
  @Input() appPreventSpecialChar: string;

  @HostListener('dragover', ['$event']) onDragover(evt) {
    evt.preventDefault();
  }
  constructor(private el: ElementRef) {}

  ngOnInit() {
    this.el.nativeElement.onkeypress = (evt) => {
      if (this.appPreventSpecialChar == 'machineName') {
        this.machineName(evt);
      } else if (evt.keyCode != 95 && !this.preventSpecialCharacters(evt)) {
        evt.preventDefault();
      }
    };
  }

  preventSpecialCharacters(e) {
    let k: number;
    e ? (k = e.keyCode) : (k = e.which);
    return (
      (k > 64 && k < 91) ||
      (k > 96 && k < 123) ||
      k == 8 ||
      k == 32 ||
      (k >= 48 && k <= 57)
    );
  }

  machineName(event) {
    if (event.target.value.length == 1 && event.keyCode === 220) {
      return;
    } else if (event.target.value.length > 1 && event.keyCode === 220) {
    }

  }
}
