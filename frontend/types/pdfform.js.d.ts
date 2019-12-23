export = pdfform;
declare function pdfform(minipdf_lib: any): any;
declare namespace pdfform {
  function transform(buf: any, fields: any): any;
}